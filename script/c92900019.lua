-- Wrong Turn
local s,id=GetID()
local COUNTER_KILL = 0x199

function s.initial_effect(c)
    -- Activate effect
    local e1=Effect.CreateEffect(c)
    e1:SetCategory(CATEGORY_COUNTER+CATEGORY_SPECIAL_SUMMON)
    e1:SetType(EFFECT_TYPE_ACTIVATE)
    e1:SetCode(EVENT_FREE_CHAIN)
    e1:SetCountLimit(1,id,EFFECT_COUNT_CODE_OATH)
    e1:SetHintTiming(0,TIMINGS_CHECK_MONSTER+TIMING_MAIN_END)
    e1:SetCondition(s.condition)
    e1:SetTarget(s.target)
    e1:SetOperation(s.activate)
    c:RegisterEffect(e1)

    -- Global tracker for opponent's Special Summons
    if not s.global_check then
        s.global_check=true
        local ge1=Effect.CreateEffect(c)
        ge1:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_CONTINUOUS)
        ge1:SetCode(EVENT_SPSUMMON_SUCCESS)
        ge1:SetOperation(s.checkop)
        Duel.RegisterEffect(ge1,0)
    end
end

-- Tracks Special Summons performed by the opponent
function s.checkop(e,tp,eg,ep,ev,re,r,rp)
    for tc in aux.Next(eg) do
        local sp=tc:GetSummonPlayer()
        Duel.RegisterFlagEffect(sp,id,RESET_PHASE+PHASE_END,0,1)
    end
end

function s.condition(e,tp,eg,ep,ev,re,r,rp)
    return Duel.GetFlagEffect(1-tp,id)>=3
end

-- Field Spell filter ("Slasher" archetype 0x991)
function s.fieldfilter(c,e,tp,eg,ep,ev,re,r,rp)
    return c:IsSetCard(0x991) and c:IsType(TYPE_FIELD) and c:GetActivateEffect():IsActivatable(tp,true,true)
end

-- Cannibal monster filter ("Cannibal" archetype 0x1991)
function s.cannibalfilter(c,e,tp)
    return c:IsSetCard(0x1991) and c:IsCanBeSpecialSummoned(e,0,tp,false,false)
end

function s.target(e,tp,eg,ep,ev,re,r,rp,chk)
    if chk==0 then
        return Duel.IsExistingMatchingCard(s.fieldfilter,tp,LOCATION_DECK+LOCATION_GRAVE,0,1,nil,e,tp,eg,ep,ev,re,r,rp)
    end
    Duel.SetOperationInfo(0,CATEGORY_COUNTER,nil,3,0,COUNTER_KILL)
    Duel.SetPossibleOperationInfo(0,CATEGORY_SPECIAL_SUMMON,nil,1,tp,LOCATION_HAND+LOCATION_DECK+LOCATION_GRAVE)
end

function s.activate(e,tp,eg,ep,ev,re,r,rp)
    -- 1. Activate 1 "Slasher" Field Spell from Deck or GY and place 3 Kill Counters
    Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_TOFIELD)
    local g=Duel.SelectMatchingCard(tp,aux.NecroValleyFilter(s.fieldfilter),tp,LOCATION_DECK+LOCATION_GRAVE,0,1,1,nil,e,tp,eg,ep,ev,re,r,rp)
    local tc=g:GetFirst()
    if tc then
        local fc=Duel.GetFieldCard(tp,LOCATION_SZONE,5)
        if fc then
            Duel.SendtoGrave(fc,REASON_RULE)
            Duel.BreakEffect()
        end
        if Duel.ActivateFieldSpell(tc,e,tp,eg,ep,ev,re,r,rp) then
            tc:AddCounter(COUNTER_KILL,3)
        end
    end

    -- 2. Then, if you control no monsters: Special Summon up to 3 "Cannibal" monsters with different names
    if Duel.GetFieldGroupCount(tp,LOCATION_MZONE,0)==0 then
        local ft=Duel.GetLocationCount(tp,LOCATION_MZONE)
        if ft>0 then
            if Duel.IsPlayerAffectedByEffect(tp,CARD_BLUEEYES_SPIRIT) then ft=1 end
            ft=math.min(ft,3)
            local sg=Duel.GetMatchingGroup(aux.NecroValleyFilter(s.cannibalfilter),tp,LOCATION_HAND+LOCATION_DECK+LOCATION_GRAVE,0,nil,e,tp)
            if #sg>0 and Duel.SelectYesNo(tp,aux.Stringid(id,0)) then
                Duel.BreakEffect()
                Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_SPSUMMON)
                local mat=aux.SelectUnselectGroup(sg,e,tp,1,ft,aux.dncheck,1,tp,HINTMSG_SPSUMMON)
                if #mat>0 then
                    Duel.SpecialSummon(mat,0,tp,tp,false,false,POS_FACEUP)
                end
            end
        end
    end

    -- 3. Extra Deck Lockdown for remainder of the turn
    local e1=Effect.CreateEffect(e:GetHandler())
    e1:SetType(EFFECT_TYPE_FIELD)
    e1:SetCode(EFFECT_CANNOT_SPECIAL_SUMMON)
    e1:SetProperty(EFFECT_FLAG_PLAYER_TARGET+EFFECT_FLAG_CLIENT_HINT)
    e1:SetDescription(aux.Stringid(id,1))
    e1:SetTargetRange(1,0)
    e1:SetTarget(s.splimit)
    e1:SetReset(RESET_PHASE+PHASE_END)
    Duel.RegisterEffect(e1,tp)
end

function s.splimit(e,c,sump,sumtype,sumpos,targetp,se)
    return c:IsLocation(LOCATION_EXTRA)
end