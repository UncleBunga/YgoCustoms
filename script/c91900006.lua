-- Arcana Guard Joker
local s,id=GetID()
function s.initial_effect(c)
    -- Link Summon Procedure
    c:EnableReviveLimit()
    Link.AddProcedure(c,s.matfilter,1,1)

    -- Effect 1: Add "Joker's Knight" on Link Summon
    local e1=Effect.CreateEffect(c)
    e1:SetDescription(aux.Stringid(id,0))
    e1:SetCategory(CATEGORY_TOHAND+CATEGORY_SEARCH)
    e1:SetType(EFFECT_TYPE_SINGLE+EFFECT_TYPE_TRIGGER_O)
    e1:SetProperty(EFFECT_FLAG_DELAY)
    e1:SetCode(EVENT_SPSUMMON_SUCCESS)
    e1:SetCountLimit(1,id)
    e1:SetCondition(s.thcon)
    e1:SetTarget(s.thtg)
    e1:SetOperation(s.thop)
    c:RegisterEffect(e1)

    -- Effect 2: Special Summon from GY based on pointing card
    local e2=Effect.CreateEffect(c)
    e2:SetDescription(aux.Stringid(id,1))
    e2:SetCategory(CATEGORY_SPECIAL_SUMMON)
    e2:SetType(EFFECT_TYPE_IGNITION)
    e2:SetRange(LOCATION_MZONE)
    e2:SetCountLimit(1,id+100) -- Fixed: Distinct integer offset prevents limit sharing
    e2:SetTarget(s.sptg)
    e2:SetOperation(s.spop)
    c:RegisterEffect(e2)

    -- Effect 3: Tribute to Set "Joker's Wild"
    local e3=Effect.CreateEffect(c)
    e3:SetDescription(aux.Stringid(id,2))
    e3:SetType(EFFECT_TYPE_IGNITION)
    e3:SetRange(LOCATION_MZONE)
    e3:SetCountLimit(1,id+200) -- Fixed: Distinct integer offset prevents limit sharing
    e3:SetCost(s.setcost)
    e3:SetTarget(s.settg)
    e3:SetOperation(s.setop)
    c:RegisterEffect(e3)
end

-- Corrected Official Card IDs for Poker Knights & Spells
local CARD_QUEENS_KNIGHT = 25652259
local CARD_KINGS_KNIGHT  = 64788463
local CARD_JACKS_KNIGHT  = 12275533
local CARD_JOKERS_KNIGHT = 29284413
local CARD_JOKERS_WILD   = 81945678

function s.matfilter(c,lc,sumtype,tp)
    return c:IsLevel(4) and c:IsAttribute(ATTRIBUTE_LIGHT,lc,sumtype,tp) and c:IsRace(RACE_WARRIOR,lc,sumtype,tp)
end

-- /// Effect 1: Search "Joker's Knight" ///
function s.thcon(e,tp,eg,ep,ev,re,r,rp)
    return e:GetHandler():IsSummonType(SUMMON_TYPE_LINK)
end
function s.thfilter(c)
    return c:IsCode(CARD_JOKERS_KNIGHT) and c:IsAbleToHand()
end
function s.thtg(e,tp,eg,ep,ev,re,r,rp,chk)
    if chk==0 then return Duel.IsExistingMatchingCard(s.thfilter,tp,LOCATION_DECK,0,1,nil) end
    Duel.SetOperationInfo(0,CATEGORY_TOHAND,nil,1,tp,LOCATION_DECK)
end
function s.thop(e,tp,eg,ep,ev,re,r,rp)
    Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_ATOHAND)
    local g=Duel.SelectMatchingCard(tp,s.thfilter,tp,LOCATION_DECK,0,1,1,nil)
    if #g>0 then
        Duel.SendtoHand(g,nil,REASON_EFFECT)
        Duel.ConfirmCards(1-tp,g)
    end
end

-- /// Effect 2: Special Summon from GY based on pointing card ///
function s.pointingfilter(c)
    return c:IsFaceup() and (c:IsCode(CARD_QUEENS_KNIGHT) or c:IsCode(CARD_KINGS_KNIGHT) or c:IsCode(CARD_JACKS_KNIGHT))
end
function s.spfilter(c,e,tp,code)
    return (c:IsCode(CARD_QUEENS_KNIGHT) or c:IsCode(CARD_KINGS_KNIGHT) or c:IsCode(CARD_JACKS_KNIGHT))
        and not c:IsCode(code) and c:IsCanBeSpecialSummoned(e,0,tp,false,false)
end
function s.sptg(e,tp,eg,ep,ev,re,r,rp,chk)
    local c=e:GetHandler()
    local lg=c:GetLinkedGroup():Filter(s.pointingfilter,nil)
    if chk==0 then
        if #lg==0 then return false end
        return Duel.GetLocationCount(tp,LOCATION_MZONE)>0 and lg:IsExists(function(tc)
            return Duel.IsExistingMatchingCard(s.spfilter,tp,LOCATION_GRAVE,0,1,nil,e,tp,tc:GetCode())
        end,1,nil)
    end
    Duel.SetOperationInfo(0,CATEGORY_SPECIAL_SUMMON,nil,1,tp,LOCATION_GRAVE)
end
function s.spop(e,tp,eg,ep,ev,re,r,rp)
    local c=e:GetHandler()
    local lg=c:GetLinkedGroup():Filter(s.pointingfilter,nil)
    if #lg==0 or Duel.GetLocationCount(tp,LOCATION_MZONE)<=0 then return end

    local valid_lg=lg:Filter(function(tc)
        return Duel.IsExistingMatchingCard(s.spfilter,tp,LOCATION_GRAVE,0,1,nil,e,tp,tc:GetCode())
    end,nil)

    if #valid_lg==0 then return end

    Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_FACEUP)
    local p_card=valid_lg:Select(tp,1,1,nil):GetFirst()
    if not p_card then return end

    local code=p_card:GetCode()
    Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_SPSUMMON)
    local g=Duel.SelectMatchingCard(tp,s.spfilter,tp,LOCATION_GRAVE,0,1,1,nil,e,tp,code)
    if #g>0 then
        Duel.SpecialSummon(g,0,tp,tp,false,false,POS_FACEUP)
    end
end

-- /// Effect 3: Tribute to Set "Joker's Wild" ///
function s.setcost(e,tp,eg,ep,ev,re,r,rp,chk)
    local c=e:GetHandler()
    if chk==0 then return c:IsLinkSummoned() and c:IsReleasable() end
    Duel.Release(c,REASON_COST)
end
function s.setfilter(c)
    return c:IsCode(CARD_JOKERS_WILD) and c:IsSSetable()
end
function s.settg(e,tp,eg,ep,ev,re,r,rp,chk)
    if chk==0 then return Duel.IsExistingMatchingCard(s.setfilter,tp,LOCATION_DECK,0,1,nil) end
end
function s.setop(e,tp,eg,ep,ev,re,r,rp)
    Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_SET)
    local g=Duel.SelectMatchingCard(tp,s.setfilter,tp,LOCATION_DECK,0,1,1,nil)
    local tc=g:GetFirst()
    if tc then
        Duel.SSet(tp,tc)
    end
end