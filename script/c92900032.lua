-- Slasher - Ancient Stalker
local s,id=GetID()
function s.initial_effect(c)
    c:EnableCounterPermit(0x199)
    -- Link Summon procedure
    c:EnableReviveLimit()
    Link.AddProcedure(c,aux.FilterBoolFunction(Card.IsSetCard,0x991),2,2)
    c:EnableCounterPermit(0x199)

    -- Search 1 "Slasher" Field or Continuous Spell on Link Summon
    local e1=Effect.CreateEffect(c)
    e1:SetDescription(aux.Stringid(id,0))
    e1:SetCategory(CATEGORY_TOHAND+CATEGORY_SEARCH)
    e1:SetType(EFFECT_TYPE_SINGLE+EFFECT_TYPE_TRIGGER_O)
    e1:SetProperty(EFFECT_FLAG_DELAY)
    e1:SetCode(EVENT_SPSUMMON_SUCCESS)
    e1:SetCondition(s.thcon)
    e1:SetCountLimit(1,id)
    e1:SetTarget(s.thtg)
    e1:SetOperation(s.thop)
    c:RegisterEffect(e1)

    -- Place 1 Kill Counter when a monster is summoned to a pointed zone
    local e2=Effect.CreateEffect(c)
    e2:SetDescription(aux.Stringid(id,1))
    e2:SetCategory(CATEGORY_COUNTER)
    e2:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_TRIGGER_O)
    e2:SetProperty(EFFECT_FLAG_DELAY)
    e2:SetCode(EVENT_SPSUMMON_SUCCESS)
    e2:SetRange(LOCATION_MZONE)
    e2:SetCondition(s.ctcon)
    e2:SetTarget(s.cttg)
    e2:SetOperation(s.ctop)
    c:RegisterEffect(e2)
    local e3=e2:Clone()
    e3:SetCode(EVENT_SUMMON_SUCCESS)
    c:RegisterEffect(e3)
end
s.counter_list={0x199}

-- Link Summon check
function s.thcon(e,tp,eg,ep,ev,re,r,rp)
    return e:GetHandler():IsSummonType(SUMMON_TYPE_LINK)
end

-- Search Filter: Slasher Field Spell or Continuous Spell
function s.thfilter(c)
    return c:IsSetCard(0x991) and c:IsSpell() and (c:IsType(TYPE_FIELD) or c:IsType(TYPE_CONTINUOUS)) and c:IsAbleToHand()
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

-- Condition: Check if summoned monster is placed in a zone this card points to
function s.ctcon(e,tp,eg,ep,ev,re,r,rp)
    local lg=e:GetHandler():GetLinkedGroup()
    return eg:IsExists(function(tc) return lg:IsContains(tc) end,1,nil)
end

-- Filter for face-up cards you control that can hold counters
function s.ctfilter(c)
    return c:IsFaceup() and c:IsCanAddCounter(0x199,1)
end

function s.cttg(e,tp,eg,ep,ev,re,r,rp,chk)
    if chk==0 then return Duel.IsExistingMatchingCard(s.ctfilter,tp,LOCATION_ONFIELD,0,1,nil) end
    Duel.SetOperationInfo(0,CATEGORY_COUNTER,nil,1,tp,LOCATION_ONFIELD)
end

function s.ctop(e,tp,eg,ep,ev,re,r,rp)
    Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_FACEUP)
    local g=Duel.SelectMatchingCard(tp,s.ctfilter,tp,LOCATION_ONFIELD,0,1,1,nil)
    local tc=g:GetFirst()
    if tc then
        tc:AddCounter(0x199,1)
    end
end